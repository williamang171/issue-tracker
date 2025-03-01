"use client";

import {
  DateField,
  MarkdownField,
  NumberField,
  Show,
  TextField,
} from "@refinedev/antd";
import { useShow } from "@refinedev/core";
import { Typography } from "antd";

const { Title } = Typography;

export default function IssueShow() {
  const { query: queryResult } = useShow({});
  const { data, isLoading } = queryResult;

  const record = data?.data;

  return (
    <Show isLoading={isLoading}>
      <Title level={5}>{"Name"}</Title>
      <TextField value={record?.name} />
      <Title level={5}>{"Description"}</Title>
      <MarkdownField value={record?.description} />
      <Title level={5}>{"Status"}</Title>
      <TextField value={record?.status} />
      <Title level={5}>{"Priority"}</Title>
      <TextField value={record?.priority} />
      <Title level={5}>{"Type"}</Title>
      <TextField value={record?.type} />
      <Title level={5}>{"Project"}</Title>
      <TextField value={record?.projectId} />
    </Show>
  );
}
