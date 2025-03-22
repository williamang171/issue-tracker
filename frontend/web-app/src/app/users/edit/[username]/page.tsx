"use client";

import { Edit, useForm, useSelect, } from "@refinedev/antd";
import { Form, Input, Select } from "antd";

export default function UserEdit() {
    const { formProps, saveButtonProps } = useForm({});

    const { selectProps: roleSelectProps } = useSelect({
        resource: 'roles',
        optionLabel: 'name',
        optionValue: 'id',
    });

    return (
        <div>
            <Edit saveButtonProps={saveButtonProps} >
                <Form {...formProps} layout="vertical">
                    <Form.Item
                        label={"UserName"}
                        name={["userName"]}
                        rules={[
                            {
                                required: true,
                            },
                        ]}
                    >
                        <Input disabled />
                    </Form.Item>
                    <Form.Item
                        label={'Role'}
                        name={'roleId'}
                        rules={[
                            {
                                required: true,
                            },
                        ]}
                    >
                        <Select {...roleSelectProps} />
                    </Form.Item>

                </Form>
            </Edit>
        </div>

    );
}
