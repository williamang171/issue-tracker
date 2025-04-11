import { Form, Input } from "antd"
import { FormText } from "../form-text"

export const LastUpdatedBy = () => {
    return (
        <Form.Item
            label={'Last Updated By'}
            name={['updatedBy']}
        >
            <FormText />
        </Form.Item>
    )
}