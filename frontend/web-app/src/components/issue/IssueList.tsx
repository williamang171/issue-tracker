'use client';

import {
    ISSUE_PRIORITY,
    ISSUE_PRIORITY_ARRAY,
} from '@app/constants/issue-priority';
import { ISSUE_STATUS, ISSUE_STATUS_ARRAY } from '@app/constants/issue-status';
import { ISSUE_TYPE, ISSUE_TYPE_ARRAY } from '@app/constants/issue-type';
import { SearchOutlined } from '@ant-design/icons';
import { renderEnumFieldLabel } from '@app/utils/utils-table-col-render';
import {
    DeleteButton,
    EditButton,
    FilterDropdown,
    getDefaultSortOrder,
    List,
    ShowButton,
    useSelect,
    useTable,
} from '@refinedev/antd';
import { getDefaultFilter, type BaseRecord } from '@refinedev/core';
import { Input, InputRef, Space, Table, theme } from 'antd';
import { RESOURCE } from '@app/constants/resource';
import { useRef } from 'react';
import { formatTimestamp } from '@app/utils/utils-dayjs';

export default function IssueList({ assignee }: { assignee?: string | null }) {

    const { tableProps, filters, sorters } = useTable({
        syncWithLocation: true,
        filters: {
            initial: [
                {
                    field: 'name',
                    operator: 'contains',
                    value: '',
                },
            ],
            permanent: assignee ? [
                {
                    field: 'assignee',
                    operator: 'eq',
                    value: assignee,
                }
            ] : [],
        },
        sorters: {
            initial: [
                {
                    field: 'createdTime',
                    order: 'desc',
                },
            ],
        },
    });

    const { token } = theme.useToken();
    const { query: queryResult } = useSelect({
        resource: RESOURCE.projects,
        optionLabel: 'name',
        optionValue: 'id',
    });
    const { query: usersQueryResult } = useSelect({
        resource: RESOURCE.users,
        optionLabel: 'userName',
        optionValue: 'userName'
    })
    const searchInput = useRef<InputRef>(null);
    const projects = queryResult?.data?.data || [];
    const users = usersQueryResult?.data?.data || [];

    return (
        <List resource='issues' breadcrumb={false}>
            <Table {...tableProps} rowKey="id">
                <Table.Column
                    dataIndex="name"
                    title={'Name'}
                    filterIcon={(filtered) => (
                        <SearchOutlined
                            style={{
                                color: filtered ? token.colorPrimary : undefined,
                            }}
                        />
                    )}
                    sorter
                    defaultFilteredValue={getDefaultFilter('name', filters, 'contains')}
                    filterDropdown={(props) => (
                        <FilterDropdown {...props}>
                            <Input placeholder={''} ref={searchInput} />
                        </FilterDropdown>
                    )}
                    filterDropdownProps={{
                        onOpenChange: (open) => {
                            if (open) {
                                setTimeout(() => searchInput.current?.select(), 100);
                            }
                        },
                    }}

                />
                <Table.Column
                    dataIndex="projectId"
                    title="Project"
                    render={(_, record) => {
                        const found = projects.find(
                            (item) => item?.id === record.projectId
                        );
                        return found?.name;
                    }}
                    defaultFilteredValue={getDefaultFilter('projectId', filters, 'eq')}
                    filterMultiple={false}
                    filters={projects.map((item) => {
                        return {
                            text: item.name,
                            value: item.id,
                        };
                    })}
                />
                <Table.Column
                    dataIndex="status"
                    title={'Status'}
                    render={(value, record) => {
                        return renderEnumFieldLabel(value, record, 'status', ISSUE_STATUS);
                    }}
                    defaultFilteredValue={getDefaultFilter('status', filters, 'eq')}
                    filters={ISSUE_STATUS_ARRAY.map((item) => {
                        return {
                            text: item.label,
                            value: item.id,
                        };
                    })}
                    filterMultiple={false}
                />
                <Table.Column
                    dataIndex="priority"
                    title={'Priority'}
                    render={(value, record) => {
                        return renderEnumFieldLabel(
                            value,
                            record,
                            'priority',
                            ISSUE_PRIORITY
                        );
                    }}
                    defaultFilteredValue={getDefaultFilter('priority', filters, 'eq')}
                    filters={ISSUE_PRIORITY_ARRAY.map((item) => {
                        return {
                            text: item.label,
                            value: item.id,
                        };
                    })}
                    filterMultiple={false}
                />
                <Table.Column
                    dataIndex="type"
                    title={'Type'}
                    render={(value, record) => {
                        return renderEnumFieldLabel(value, record, 'type', ISSUE_TYPE);
                    }}
                    defaultFilteredValue={getDefaultFilter('type', filters, 'eq')}
                    filterMultiple={false}
                    filters={ISSUE_TYPE_ARRAY.map((item) => {
                        return {
                            text: item.label,
                            value: item.id,
                        };
                    })}
                    filterDropdownProps={{
                        onOpenChange: (open) => {
                            if (open) {
                                setTimeout(() => searchInput.current?.select(), 100);
                            }
                        },
                    }}
                />
                {assignee ? null : <Table.Column
                    dataIndex="assignee"
                    title={'Assignee'}
                    render={(value, record) => {
                        return record.assignee || '-';
                    }}
                    defaultFilteredValue={getDefaultFilter('assignee', filters, 'eq')}
                    filterMultiple={false}
                    filters={users.map((item) => {
                        return {
                            text: item.userName,
                            value: item.userName,
                        };
                    })}
                />}
                <Table.Column
                    dataIndex="createdTime"
                    title="Created At"
                    render={(value) => {
                        if (!value) return '-';
                        return <div>{formatTimestamp(value)}</div>;
                    }}
                    sorter
                    defaultSortOrder={getDefaultSortOrder('createdTime', sorters)}
                />
                <Table.Column
                    title={'Actions'}
                    dataIndex="actions"
                    render={(_, record: BaseRecord) => (
                        <Space>
                            <EditButton size="small" recordItemId={record.id} icon={null} type='link'>
                                Details
                            </EditButton>
                            <DeleteButton size="small" recordItemId={record.id} icon={null} type='link' />
                        </Space>
                    )}
                />
            </Table>
        </List>
    );
}
