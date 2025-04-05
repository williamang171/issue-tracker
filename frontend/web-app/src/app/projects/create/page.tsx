'use client';

import { GoBack } from '@components/goback';
import { Create, useForm } from '@refinedev/antd';
import { Form, Input } from 'antd';

export default function ProjectCreate() {
  const { formProps, saveButtonProps } = useForm({});

  return (
    <Create saveButtonProps={saveButtonProps} breadcrumb={false}
      title={
        <GoBack
          goBackText='Projects'
          title='Create Project'
          href='/projects'
        />
      }
      goBack={null}
    >
      <Form {...formProps} layout="vertical">
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
      </Form>
    </Create>
  );
}
