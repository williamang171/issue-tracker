import { Empty } from "antd"

export const EmptyPlaceholder = () => {
    return (<div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '300px' }}>
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} />
    </div>)
}