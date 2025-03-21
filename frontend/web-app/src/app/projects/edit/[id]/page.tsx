"use client";

import { DashboardCharts } from "@components/dashboard";
import { UsersList } from "@components/project-assignments/List";
import { useGetProjectChartsData } from "@hooks/useGetProjectChartsData";
import { Edit, useForm, } from "@refinedev/antd";
import { Form, Input } from "antd";

export default function ProjectEdit() {
  const { formProps, saveButtonProps, query: queryResult } = useForm({});
  const { issuePriorityCountData, issueStatusCountData, issueTypeCountData } = useGetProjectChartsData();

  return (
    <div>
      <Edit saveButtonProps={saveButtonProps} >
        <Form {...formProps} layout="vertical">
          <Form.Item
            label={"Name"}
            name={["name"]}
            rules={[
              {
                required: true,
              },
            ]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            label={"Description"}
            name="description"
            rules={[
              {
                required: true,
              },
            ]}
          >
            <Input.TextArea rows={5} />
          </Form.Item>

        </Form>
      </Edit>
      <div style={{ marginBottom: "24px" }} />
      {queryResult?.data?.data.id ? <UsersList projectId={queryResult?.data?.data.id} /> : null}

      <div style={{ marginBottom: "24px" }} />
      <DashboardCharts
        issuePriorityCountData={issuePriorityCountData}
        issueStatusCountData={issueStatusCountData}
        issueTypeCountData={issueTypeCountData}
      />
    </div>

  );
}
