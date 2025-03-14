"use client";

import {
  DeleteButton,
  EditButton,
  FilterDropdown,
  List,
  ShowButton,
  useTable,
} from "@refinedev/antd";
import { getDefaultFilter, type BaseRecord } from "@refinedev/core";
import { SearchOutlined } from '@ant-design/icons';
import { Input, Space, Table, theme } from "antd";

export default function ProjectList() {
  const { tableProps, filters } = useTable({
    syncWithLocation: true,
    filters: {
      initial: [
        {
          field: 'name',
          operator: 'contains',
          value: '',
        }
      ]
    }
  });
  const { token } = theme.useToken();

  return (
    <List>
      <Table {...tableProps} rowKey="id">
        <Table.Column dataIndex="name" title={"Name"} sorter filterIcon={(filtered) => (
          <SearchOutlined
            style={{
              color: filtered ? token.colorPrimary : undefined,
            }}
          />
        )}

          defaultFilteredValue={getDefaultFilter('name', filters, 'contains')}
          filterDropdown={(props) => (
            <FilterDropdown
              {...props}
            >
              <Input placeholder={''} />
            </FilterDropdown>
          )} />
        <Table.Column dataIndex="description" title={"Description"} />
        <Table.Column
          title={"Actions"}
          dataIndex="actions"
          render={(_, record: BaseRecord) => (
            <Space>
              <EditButton hideText size="small" recordItemId={record.id} />
              <ShowButton hideText size="small" recordItemId={record.id} />
              <DeleteButton hideText size="small" recordItemId={record.id} />
            </Space>
          )}
        />
      </Table>
    </List>
  );
}
