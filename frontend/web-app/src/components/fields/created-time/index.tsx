import { Form, Input } from "antd"
import { formatTimestamp } from "@app/utils/utils-dayjs";
import { FormText } from "../form-text";

export const CreatedTime = () => {
    return (
        <Form.Item
            label={'Created Time'}
            name={['createdTime']}
            getValueProps={(value) => ({ value: formatTimestamp(value) })}
        >
            <FormText />
        </Form.Item>
    )
}