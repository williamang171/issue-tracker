import {
    Form,
    Select,
    Modal,
    Spin,
    ModalProps,
} from "antd";
import { FormProps } from "antd/lib";
import { useSelect } from "@refinedev/antd";
import { useModalCreateStyle } from "@hooks/useModalCreateStyle";
import { BaseKey, useList } from "@refinedev/core";
import { useEffect, useMemo } from "react";
import { RESOURCE } from "@app/constants/resource";

export function ModalAddUsersToProject({
    createModalProps,
    createFormLoading,
    createFormProps,
    projectId
}: {
    createModalProps: ModalProps,
    createFormLoading: boolean,
    createFormProps: FormProps,
    projectId?: BaseKey
}) {

    const modalStyle = useModalCreateStyle();
    const { selectProps: userSelectProps } = useSelect({
        resource: "users",
        optionLabel: 'userName',
        optionValue: 'userName',
        filters: [
            {
                field: 'projectId',
                value: projectId,
                operator: 'eq'
            }
        ]
    });

    const { data, refetch } = useList({
        resource: `${RESOURCE.projectAssignments}/all`,
        filters: [
            {
                field: 'projectId',
                value: projectId,
                operator: 'eq'
            }
        ]
    });
    const projectAssignments = data?.data || [];

    useEffect(() => {
        if (createModalProps.open) {
            refetch();
        }
    }, [createModalProps.open, refetch])

    const usersSet = useMemo(() => {
        const s = new Set();
        projectAssignments.forEach((pa) => {
            s.add(pa.userName);
        });
        return s;
    }, [projectAssignments]);

    const newOptions = useMemo(() => {
        const newOptions = userSelectProps.options?.map((o) => {
            if (usersSet.has(o.value)) {
                return {
                    ...o,
                    disabled: true
                }
            }
            return o;
        }) || [];
        return newOptions;
    }, [userSelectProps.options, usersSet])

    return (
        <Modal {...createModalProps} styles={modalStyle} title="Add users to project">
            <Spin spinning={createFormLoading}>
                <Form {...createFormProps} layout="vertical">
                    <Form.Item
                        label="Users"
                        name="userNames"
                        rules={[
                            {
                                required: true,
                            },
                        ]}
                    >
                        <Select mode="multiple"
                            {...userSelectProps}
                            options={newOptions}
                        />
                    </Form.Item>
                    <Form {...createFormProps} layout="vertical" style={{ display: 'none' }}>
                        <Form.Item
                            name="projectId"
                            rules={[
                                {
                                    required: true,
                                },
                            ]}
                        >
                            <Select value={projectId} />
                        </Form.Item>
                    </Form>
                </Form>
            </Spin>
        </Modal>
    )
}