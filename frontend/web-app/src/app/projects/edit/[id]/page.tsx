"use client";

import { Edit, useForm, } from "@refinedev/antd";
import { Form, Input } from "antd";

export default function ProjectEdit() {
  const { formProps, saveButtonProps, query: queryResult } = useForm({});

  return (
    <Edit saveButtonProps={saveButtonProps}>
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
  );
}
