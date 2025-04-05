import { createConstantObject } from "../utils/utils-constants";

export const BUG = 0;
export const FEATURE_REQUEST = 1;
export const OTHER = 2;

export const ISSUE_TYPE = {
    [BUG]: createConstantObject(BUG, 'Bug', '#E53935'),
    [FEATURE_REQUEST]: createConstantObject(
        FEATURE_REQUEST,
        'Feature Request',
        '#009688'
    ),
    [OTHER]: createConstantObject(OTHER, 'Other', '#9C27B0'),
} as const;

export type Type = typeof ISSUE_TYPE[keyof typeof ISSUE_TYPE];

export const ISSUE_TYPE_ARRAY: Type[] = Object.values(ISSUE_TYPE);

export function isKeyOfProjectTaskType(key: number): key is keyof typeof ISSUE_TYPE {
    return key in ISSUE_TYPE;
}