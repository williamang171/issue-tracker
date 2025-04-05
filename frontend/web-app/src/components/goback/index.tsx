import { LeftOutlined } from "@ant-design/icons"
import { Button } from "antd"
import Link from "next/link"

export const GoBack = ({ title, href }: { title: string, href: string }) => {
    return (
        <div style={{ display: 'flex' }}>
            <Link
                href={href}
            >
                <Button type="text" icon={<LeftOutlined />} />
            </Link>
            <div style={{ marginLeft: "24px" }} >
                {title}
            </div>
        </div>
    )
}