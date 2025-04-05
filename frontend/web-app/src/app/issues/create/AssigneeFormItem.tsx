import { useSelect } from "@refinedev/antd";
import { useList } from "@refinedev/core";
import { Form, Select } from "antd";
import { FormInstance } from "antd/lib";
import { useEffect } from "react";

export default function AssigneeFormItem({ form }: { form: FormInstance<{ projectId: string, assignee: string }> }) {
    const projectId = Form.useWatch('projectId', form);
    const assignee = Form.useWatch('assignee', form);

    const { selectProps: assigneeSelectProps, } = useSelect({
        resource: "projectAssignments/all",
        optionLabel: "userName",
        optionValue: "userName",
        filters: [
            {
                field: 'projectId',
                operator: 'eq',
                value: projectId,
            },
        ]
    });
    const usernames = assigneeSelectProps.options;


    useEffect(() => {
        if (!usernames) {
            return;
        }
        const found = usernames?.find((username) => {
            return username.value === assignee;
        });
        if (!found) {
            form.setFieldValue("assignee", null);
        }
    }, [usernames, assignee, form.setFieldValue])

    return (
        <Form.Item
            label={"Assignee"}
            name={["assignee"]}
        >
            <Select
                {...assigneeSelectProps}
                style={{ width: 200 }}
                allowClear
            />
        </Form.Item>
    )
}