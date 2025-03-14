"use client";

import { ISSUE_PRIORITY, ISSUE_PRIORITY_ARRAY } from "@app/constants/issue-priority";
import { ISSUE_STATUS, ISSUE_STATUS_ARRAY } from "@app/constants/issue-status";
import { ISSUE_TYPE, ISSUE_TYPE_ARRAY } from "@app/constants/issue-type";
import { SearchOutlined } from '@ant-design/icons';
import { renderEnumFieldLabel } from "@app/utils/utils-table-col-render";
import {
  DeleteButton,
  EditButton,
  FilterDropdown,
  List,
  ShowButton,
  useSelect,
  useTable,
} from "@refinedev/antd";
import { getDefaultFilter, type BaseRecord } from "@refinedev/core";
import { Input, Space, Table, theme } from "antd";
import { RESOURCE } from "@app/constants/resource";

export default function IssueList() {
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

  const { query: projectQueryResult } = useSelect({
    resource: "projects",
    optionLabel: 'name',
    optionValue: 'id',
  });
  const { token } = theme.useToken();
  const { query: queryResult } = useSelect({
    resource: RESOURCE.projects,
    optionLabel: 'name',
    optionValue: 'id',
  });
  const projects = queryResult?.data?.data || [];

  return (
    <List>
      <Table {...tableProps} rowKey="id">
        <Table.Column dataIndex="name" title={"Name"}
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
            <FilterDropdown
              {...props}
            >
              <Input placeholder={''} />
            </FilterDropdown>
          )}
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
          defaultFilteredValue={getDefaultFilter(
            'projectId',
            filters,
            'eq'
          )}
          filterMultiple={false}
          filters={projects.map((item) => {
            return {
              text: item.name,
              value: item.id,
            };
          })}
        />
        <Table.Column dataIndex="status" title={"Status"}
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
        <Table.Column dataIndex="priority" title={"Priority"}
          render={(value, record) => {
            return renderEnumFieldLabel(value, record, 'priority', ISSUE_PRIORITY);
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
        <Table.Column dataIndex="type" title={"Type"}
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
        />
        <Table.Column dataIndex="projectId" title={"Project"} render={(value, record) => {
          const projects = projectQueryResult?.data?.data || [];
          const found = projects.find(
            (item) => item?.id === record.projectId
          );
          return found?.name;
        }} />
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
