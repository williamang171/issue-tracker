import { Form } from "antd"
import { FormText } from "../form-text"

export const CreatedBy = () => {
    return (
        <Form.Item
            label={'Created By'}
            name={['createdBy']}
        >
            <FormText />
        </Form.Item>
    )
}