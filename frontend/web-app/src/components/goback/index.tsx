import { LeftOutlined } from "@ant-design/icons"
import { Button } from "antd"
import Link from "next/link"

export const GoBack = ({ title, goBackText, href }: { goBackText: string, title: string, href: string }) => {
    return (
        <div style={{ display: 'flex' }}>
            <Link
                href={href}
            >
                <Button type="default" icon={<LeftOutlined />}>
                    {goBackText}
                </Button>
            </Link>
            <div style={{ marginLeft: "24px" }} >
                {title}
            </div>
        </div>
    )
}