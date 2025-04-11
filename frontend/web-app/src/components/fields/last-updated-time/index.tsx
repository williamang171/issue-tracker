import { Form, Input } from "antd"
import { formatTimestamp } from "@app/utils/utils-dayjs";
import { FormText } from "../form-text";

export const LastUpdatedTime = () => {
    return (
        <Form.Item
            label={'Last Updated Time'}
            name={['updatedTime']}
            getValueProps={(value) => ({ value: formatTimestamp(value) })}
        >
            <FormText />
        </Form.Item>
    )
}