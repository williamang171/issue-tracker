'use client';

import {
    DeleteButton,
    EditButton,
    FilterDropdown,
    List,
    ShowButton,
    useTable,
} from '@refinedev/antd';
import { getDefaultFilter, useList, type BaseRecord } from '@refinedev/core';
import { SearchOutlined } from '@ant-design/icons';
import { Input, InputRef, Space, Table, theme } from 'antd';
import { useRef } from 'react';
import { RESOURCE } from '@app/constants/resource';

export default function UserList() {
    const searchInput = useRef<InputRef>(null);
    const { tableProps, filters } = useTable({
        syncWithLocation: true,
        filters: {
            initial: [
                {
                    field: 'userName',
                    operator: 'contains',
                    value: '',
                },
            ],
        },
    });
    const { token } = theme.useToken();
    const { data } = useList({
        resource: `${RESOURCE.roles}`,
    });
    const roles = data?.data || [];

    return (
        <List canCreate={false}>
            <Table {...tableProps} rowKey="id">
                <Table.Column
                    dataIndex="userName"
                    title={'UserName'}
                    sorter
                    filterIcon={(filtered) => (
                        <SearchOutlined
                            style={{
                                color: filtered ? token.colorPrimary : undefined,
                            }}
                        />
                    )}
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
                    dataIndex="roleId"
                    title={'Role'}
                    sorter
                    render={(roleId) => {
                        const role = roles.find((r) => r.id === roleId);
                        return (<div>
                            {role?.name}
                        </div>)
                    }}
                />

                <Table.Column
                    title={'Actions'}
                    dataIndex="actions"
                    render={(_, record: BaseRecord) => (
                        <Space>
                            <EditButton hideText size="small" recordItemId={record.userName} />
                            <ShowButton hideText size="small" recordItemId={record.id} />
                            <DeleteButton hideText size="small" recordItemId={record.id} />
                        </Space>
                    )}
                />
            </Table>
        </List>
    );
}
