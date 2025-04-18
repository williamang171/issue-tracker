import React from 'react';
import { Card, Image, List, Space } from 'antd';
import { useCan, useList } from '@refinedev/core';
import { useParams } from 'next/navigation';
import { Text } from '../text';
import UploadAttachment from './uploadAttachment';
import { DeleteButton } from '@refinedev/antd';
import { RESOURCE } from '@app/constants/resource';

const resource = RESOURCE.attachments;

const App: React.FC = () => {
  const params = useParams();
  const { data } = useList({
    resource: resource,
    pagination: {
      mode: 'off',
    },
    sorters: [
      {
        field: 'createdTime',
        order: 'desc',
      },
    ],
    filters: [{ field: 'issueId', operator: 'eq', value: params.id }],
  });

  const renderPictures = () => {
    return (
      <List
        itemLayout="horizontal"
        dataSource={data?.data}
        renderItem={(item, index) => (
          <List.Item>
            <List.Item.Meta
              avatar={
                <Image
                  width={100}
                  height={'auto'}
                  src={item.url}
                  style={{
                    maxHeight: '100px',
                  }}
                />
              }
              description={
                <div
                  style={{ display: 'flex', justifyContent: 'space-between' }}
                >
                  <div>{item.name}</div>
                  <DeleteButton
                    style={{ marginRight: '8px' }}
                    resource="attachments"
                    recordItemId={item.id}
                    hideText
                    size="small"
                  />
                </div>
              }
            />
          </List.Item>
        )}
      />
    );
  };

  return (
    <Card
      bodyStyle={{
        padding: '0',
      }}
      title={
        <Space size={16}>
          <Text>Attachments</Text>
        </Space>
      }
      extra={<UploadAttachment />}
    >
      {renderPictures()}
    </Card>
  );
};

export default App;
