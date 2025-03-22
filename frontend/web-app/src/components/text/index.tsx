import type { FC } from 'react';

import { Typography } from 'antd';

export type TextProps = {
  size?:
    | 'xs'
    | 'sm'
    | 'md'
    | 'lg'
    | 'xl'
    | 'xxl'
    | 'xxxl'
    | 'huge'
    | 'xhuge'
    | 'xxhuge';
} & React.ComponentProps<typeof Typography.Text>;

export const Text: FC<TextProps> = ({ size = 'sm', children, ...rest }) => {
  return <Typography.Text {...rest}>{children}</Typography.Text>;
};
