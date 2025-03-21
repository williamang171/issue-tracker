import React, { useEffect, useMemo } from 'react';
import { BaseKey, CrudFilter, getDefaultFilter, useCan, useInvalidate } from '@refinedev/core';

import {
    List,
    useTable,
    DeleteButton,
    FilterDropdown,
    getDefaultSortOrder,
    useModalForm,
    CreateButton,
} from '@refinedev/antd';
import { SearchOutlined } from '@ant-design/icons';
import { Table, Space, theme, Input } from 'antd';
import { ModalAddUsersToProject } from './ModalAddUsersToProject';
import { RESOURCE } from '@app/constants/resource';
import { useList } from "@refinedev/core";

const resource = RESOURCE.projectAssignments

export const UsersList = ({ projectId }: { projectId: BaseKey }) => {
    const { token } = theme.useToken();
    const invalidate = useInvalidate();

    const { data: canAccess } = useCan({
        resource: resource,
        action: "delete",
        params: {},
    });

    const { data } = useList({
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

    // Create Modal
    const {
        modalProps: createModalProps,
        formProps: createFormProps,
        formLoading: createFormLoading,
        show: createModalShow,
        form
    } = useModalForm({
        action: 'create',
        syncWithLocation: false,
        resource: `${resource}/bulk`,
        successNotification: {
            description: 'Success',
            message: 'Added users to project',
            type: 'success'
        },
        onMutationSuccess() {
            invalidate({ invalidates: ['list'], resource: RESOURCE.projectAssignments });
            invalidate({ invalidates: ['list'], resource: 'all' });
        },
    });

    useEffect(() => {
        if (createModalProps.open) {
            form.setFieldValue('projectId', projectId);
        }
    }, [createModalProps.open, form, form.setFieldValue, projectId]);

    const permanentFilters: CrudFilter[] = useMemo(() => {
        if (projectId) {
            return [
                {
                    field: 'projectId',
                    operator: 'eq',
                    value: projectId,
                },
            ];
        }
        return [];
    }, [projectId]);

    const { tableProps, filters } = useTable({
        resource: resource,
        syncWithLocation: false,
        filters: {
            permanent: permanentFilters,
            initial: [
                {
                    field: 'userName',
                    operator: 'contains',
                    value: '',
                },
            ],
        },
    });

    const additionalListProps = { breadcrumb: null };

    return (
        <>
            <List
                title="Users"
                resource={RESOURCE.projectAssignments}
                createButtonProps={{
                    onClick: () => {
                        createModalShow();
                    },
                }}
                {...additionalListProps}
                headerButtons={({ createButtonProps }) => (
                    <>
                        {createButtonProps && (
                            <CreateButton {...createButtonProps}>Add Users</CreateButton>
                        )}
                    </>
                )}
            >
                <Table {...tableProps} rowKey="id">
                    <Table.Column
                        dataIndex="userName"
                        title="Username"
                        render={(_, row) => {
                            return row.userName;
                        }}
                        filterIcon={(filtered) => (
                            <SearchOutlined
                                style={{
                                    color: filtered ? token.colorPrimary : undefined,
                                }}
                            />
                        )}
                        defaultFilteredValue={getDefaultFilter(
                            'userName',
                            filters,
                            'contains'
                        )}
                        filterDropdown={(props) => (
                            <FilterDropdown
                                {...props}
                            >
                                <Input placeholder={''} />
                            </FilterDropdown>
                        )}


                    />
                    <Table.Column
                        title="Actions"
                        dataIndex="actions"
                        key="actions"
                        render={(_, record) => (
                            <Space>
                                <DeleteButton
                                    disabled={!canAccess?.can}
                                    size="small"
                                    recordItemId={record.id}
                                    icon={null}
                                    resource={RESOURCE.projectAssignments}
                                    successNotification={{
                                        description: 'Success',
                                        message: 'Removed user from project',
                                        type: 'success'
                                    }}
                                    onSuccess={() => {
                                        invalidate({ invalidates: ['list'], resource: RESOURCE.usersProjectUnassigned })
                                    }}
                                >
                                    Unassign
                                </DeleteButton>
                            </Space>
                        )}
                    />
                </Table>
            </List>
            <ModalAddUsersToProject
                createFormLoading={createFormLoading}
                createFormProps={createFormProps}
                createModalProps={createModalProps}
                projectId={projectId}
            />
        </>
    );
};
