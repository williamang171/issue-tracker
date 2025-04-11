import { green, yellow, orange, red } from '@ant-design/colors';
import { createConstantObject } from "../utils/utils-constants";


export const LOW = 0;
export const MEDIUM = 1;
export const HIGH = 2;
export const CRITICAL = 3;

export const ISSUE_PRIORITY = {
    [LOW]: createConstantObject(LOW, 'Low', green.primary),
    [MEDIUM]: createConstantObject(MEDIUM, 'Medium', yellow.primary),
    [HIGH]: createConstantObject(HIGH, 'High', orange.primary),
    [CRITICAL]: createConstantObject(CRITICAL, 'Critical', red.primary),
} as const;

export type Priority = typeof ISSUE_PRIORITY[keyof typeof ISSUE_PRIORITY];

export const ISSUE_PRIORITY_ARRAY: Priority[] = Object.values(ISSUE_PRIORITY);

export function isKeyOfProjectTaskPriority(key: number): key is keyof typeof ISSUE_PRIORITY {
    return key in ISSUE_PRIORITY;
}
