'use client';

import { Create, useForm, useSelect } from '@refinedev/antd';
import { Form, Input, InputNumber, Select } from 'antd';
import { ISSUE_STATUS_ARRAY } from '@app/constants/issue-status';
import { ISSUE_TYPE_ARRAY } from '@app/constants/issue-type';
import { mapToSelectItemObject } from '@app/utils/uitils-select';
import { ISSUE_PRIORITY_ARRAY } from '@app/constants/issue-priority';
import AssigneeFormItem from './AssigneeFormItem';
import { useRouter } from 'next/compat/router';
import { useBack } from '@refinedev/core';

export default function IssueCreate() {
  const back = useBack();
  const { formProps, saveButtonProps, form } = useForm<any, any, any>({});
  const { selectProps: projectSelectProps } = useSelect({
    resource: 'projects/all',
    optionLabel: 'name',
    optionValue: 'id',
  });

  return (
    <Create saveButtonProps={saveButtonProps}>
      <Form {...formProps} layout="vertical">
        <Form.Item
          label={'Project'}
          name={'projectId'}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <Select {...projectSelectProps} />
        </Form.Item>
        <Form.Item
          label={'Name'}
          name={['name']}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          label={'Description'}
          name="description"
          rules={[
            {
              required: true,
            },
          ]}
        >
          <Input.TextArea rows={5} />
        </Form.Item>
        <Form.Item
          label={'Status'}
          name={['status']}
          initialValue={0}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <Select
            defaultValue={0}
            options={ISSUE_STATUS_ARRAY.map(mapToSelectItemObject)}
            style={{ width: 200 }}
          />
        </Form.Item>
        <Form.Item
          label={'Priority'}
          name={['priority']}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <Select
            options={ISSUE_PRIORITY_ARRAY.map(mapToSelectItemObject)}
            style={{ width: 200 }}
          />
        </Form.Item>
        <Form.Item
          label={'Type'}
          name={['type']}
          rules={[
            {
              required: true,
            },
          ]}
        >
          <Select
            options={ISSUE_TYPE_ARRAY.map(mapToSelectItemObject)}
            style={{ width: 200 }}
          />
        </Form.Item>
        <AssigneeFormItem form={form} />
      </Form>
    </Create>
  );
}
