import { useLink } from "@refinedev/core";
import { theme, Typography } from "antd";

import { Logo } from "./styled";

type TitleProps = {
    collapsed: boolean;
};

export const Title: React.FC<TitleProps> = ({ collapsed }) => {
    const { token } = theme.useToken();
    const Link = useLink();

    return (
        <Logo>
            <Link to="/">

                <div style={{ maxHeight: '28px', display: 'flex', alignItems: 'center' }}  >


                    <Typography.Title level={4} style={{ marginBottom: '0px', marginLeft: "12px" }} >Issue Tracker</Typography.Title>
                </div>

            </Link>
        </Logo>
    );
};