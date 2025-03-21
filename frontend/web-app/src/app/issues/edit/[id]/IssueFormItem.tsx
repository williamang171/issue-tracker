import { useSelect } from "@refinedev/antd";
import { useList } from "@refinedev/core";
import { Form, Select } from "antd";
import { FormInstance } from "antd/lib";

export default function IssueFormItem({ projectId }: { projectId: string }) {
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

    return (
        <Form.Item
            label={"Assignee"}
            name={["assignee"]}

        >
            <Select
                {...assigneeSelectProps}
                style={{ width: 200 }}
            />
        </Form.Item>
    )
}