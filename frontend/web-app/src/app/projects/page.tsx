'use client';

import {
  DeleteButton,
  EditButton,
  FilterDropdown,
  getDefaultSortOrder,
  List,
  ShowButton,
  useTable,
} from '@refinedev/antd';
import { getDefaultFilter, type BaseRecord } from '@refinedev/core';
import { EyeOutlined, SearchOutlined } from '@ant-design/icons';
import { Input, InputRef, Space, Table, theme } from 'antd';
import { useRef } from 'react';
import { formatTimestamp } from '@app/utils/utils-dayjs';

export default function ProjectList() {
  const searchInput = useRef<InputRef>(null);
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

  return (
    <List>
      <Table {...tableProps} rowKey="id">
        <Table.Column
          dataIndex="name"
          title={'Name'}
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
        <Table.Column dataIndex="description" title={'Description'} />
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
              <EditButton size="small" recordItemId={record.id} hideText icon={<EyeOutlined />} >
              </EditButton>

            </Space>
          )}
        />
      </Table>
    </List>
  );
}
