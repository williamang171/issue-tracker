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
import { EyeOutlined, SearchOutlined } from '@ant-design/icons';
import { Input, InputRef, Space, Table, theme } from 'antd';
import { useRef } from 'react';
import { RESOURCE } from '@app/constants/resource';
import { useGetUserRole } from '@hooks/useGetUserRole';

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

    const { isAdmin } = useGetUserRole();
    if (!isAdmin) {
        return null;
    }

    return (
        <List canCreate={false}>
            <Table {...tableProps} rowKey="id">
                <Table.Column
                    dataIndex="userName"
                    title={'UserName'}

                    filterIcon={(filtered) => (
                        <SearchOutlined
                            style={{
                                color: filtered ? token.colorPrimary : undefined,
                            }}
                        />
                    )}
                    defaultFilteredValue={getDefaultFilter('userName', filters, 'contains')}
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
                            <EditButton size="small" recordItemId={record.userName} hideText icon={<EyeOutlined />} />
                        </Space>
                    )}
                />
            </Table>
        </List>
    );
}
