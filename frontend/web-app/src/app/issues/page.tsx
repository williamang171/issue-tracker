"use client";

import { ISSUE_PRIORITY } from "@app/constants/issue-priority";
import { isKeyOfProjectIssueStatus, ISSUE_STATUS } from "@app/constants/issue-status";
import { ISSUE_TYPE } from "@app/constants/issue-type";
import { renderEnumFieldLabel } from "@app/utils/utils-table-col-render";
import {
  DeleteButton,
  EditButton,
  List,
  ShowButton,
  useSelect,
  useTable,
} from "@refinedev/antd";
import { type BaseRecord } from "@refinedev/core";
import { Space, Table } from "antd";

export default function IssueList() {
  const { tableProps } = useTable({
    syncWithLocation: true,
  });

  const { selectProps: projectSelectProps, query: projectQueryResult } = useSelect({
    resource: "projects",
    optionLabel: 'name',
    optionValue: 'id',
  });

  return (
    <List>
      <Table {...tableProps} rowKey="id">
        <Table.Column dataIndex="name" title={"Name"} />
        <Table.Column dataIndex="description" title={"Description"} />
        <Table.Column dataIndex="status" title={"Status"}
          render={(value, record) => {
            return renderEnumFieldLabel(value, record, 'status', ISSUE_STATUS);
          }}
        />
        <Table.Column dataIndex="priority" title={"Priority"} render={(value, record) => {
          return renderEnumFieldLabel(value, record, 'priority', ISSUE_PRIORITY);
        }} />
        <Table.Column dataIndex="type" title={"Type"} render={(value, record) => {
          return renderEnumFieldLabel(value, record, 'type', ISSUE_TYPE);
        }} />
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
