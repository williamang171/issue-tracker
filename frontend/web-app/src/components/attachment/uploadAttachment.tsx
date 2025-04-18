import React, { useState } from 'react';
import { UploadOutlined } from '@ant-design/icons';
import type { UploadProps } from 'antd';
import { Button, message, Upload } from 'antd';
import type { GetProp } from 'antd';

import { API_URL } from "@providers/data-provider/data-provider.client";
import { useParams } from 'next/navigation';
import { useInvalidate } from '@refinedev/core';
import { RESOURCE } from '@app/constants/resource';
import { useSession } from 'next-auth/react';
import { useGetUserRole } from '@hooks/useGetUserRole';

type FileType = Parameters<GetProp<UploadProps, 'beforeUpload'>>[0];

const beforeUpload = (file: FileType) => {
    const isJpgOrPng = file.type === 'image/jpeg' || file.type === 'image/png';
    if (!isJpgOrPng) {
        message.error('You can only upload images!');
        return false;
    }
    const isLt2M = file.size / 1024 / 1024 < 2;
    if (!isLt2M) {
        message.error('File size must smaller than 2MB!');
        return false;
    }
    return isJpgOrPng && isLt2M;
};


const UploadAttachment: React.FC = () => {
    const [isLoading, setIsLoading] = useState(false);
    const { data, status } = useSession();
    const params = useParams();
    const invalidate = useInvalidate();
    const { isReadOnly } = useGetUserRole();
    const props: UploadProps = {
        action: `${API_URL}/upload`,
        headers: {
            authorization: `Bearer ${data?.accessToken}`
        },
        async onChange(info) {
            if (info.file.status === 'uploading') {
                setIsLoading(true);
            }
            if (info.file.status !== 'uploading') {
                console.log(info.file, info.fileList);
                setIsLoading(false);
            }
            if (info.file.status === 'done') {
                // Attach image to issue
                try {
                    const infoResponse = info.file.response;
                    await fetch(`${API_URL}/attachments`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            authorization: `Bearer ${data?.accessToken}`
                        },
                        body: JSON.stringify({
                            issueId: params.id?.toString(),
                            name: info.file.name,
                            url: infoResponse.url,
                            publicId: infoResponse.publicId
                        }),
                    }).then((data) => {
                        if (data.ok) {
                            message.success(`${info.file.name} file uploaded successfully`);
                        }
                    });
                    invalidate({ invalidates: ['list'], resource: RESOURCE.attachments });
                    setIsLoading(false);
                } catch (error) {
                    console.log(error);
                }
            } else if (info.file.status === 'error') {
                message.error(`${info.file.name} file upload failed.`);
            }
        },
        showUploadList: false
    }
    return (
        <Upload {...props} beforeUpload={beforeUpload} accept="image/png, image/jpeg">
            <Button disabled={isReadOnly} loading={isLoading} icon={<UploadOutlined />}>Attach</Button>
        </Upload>
    );
}

export default UploadAttachment;