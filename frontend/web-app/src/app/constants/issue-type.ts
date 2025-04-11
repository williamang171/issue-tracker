import { purple, red, blue } from '@ant-design/colors';
import { createConstantObject } from "../utils/utils-constants";

export const BUG = 0;
export const FEATURE_REQUEST = 1;
export const OTHER = 2;

export const ISSUE_TYPE = {
    [BUG]: createConstantObject(BUG, 'Bug', red.primary),
    [FEATURE_REQUEST]: createConstantObject(
        FEATURE_REQUEST,
        'Feature Request',
        blue.primary
    ),
    [OTHER]: createConstantObject(OTHER, 'Other', purple.primary),
} as const;

export type Type = typeof ISSUE_TYPE[keyof typeof ISSUE_TYPE];

export const ISSUE_TYPE_ARRAY: Type[] = Object.values(ISSUE_TYPE);

export function isKeyOfProjectTaskType(key: number): key is keyof typeof ISSUE_TYPE {
    return key in ISSUE_TYPE;
}