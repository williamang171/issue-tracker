'use client';

import { GoBack } from '@components/goback';
import { Edit, useForm, useSelect } from '@refinedev/antd';
import { Form, Input, Select, Switch } from 'antd';

export default function UserEdit() {
  const { formProps, saveButtonProps } = useForm({});

  const { selectProps: roleSelectProps } = useSelect({
    resource: 'roles',
    optionLabel: 'name',
    optionValue: 'id',
  });

  return (
    <div>
      <Edit
        title={
          <GoBack
            goBackText='Users'
            title='User Details'
            href='/users'
          />
        }
        goBack={null}
        saveButtonProps={saveButtonProps}
        breadcrumb={false}
        headerButtons={<div />}
      >
        <Form {...formProps} layout="vertical">
          <Form.Item
            label={'UserName'}
            name={['userName']}
            rules={[
              {
                required: true,
              },
            ]}
          >
            <Input disabled />
          </Form.Item>
          <Form.Item
            label={'Role'}
            name={'roleId'}
            rules={[
              {
                required: true,
              },
            ]}
          >
            <Select {...roleSelectProps} />
          </Form.Item>
          <Form.Item label={'Is Active'} name={'isActive'}>
            <Switch />
          </Form.Item>
        </Form>
      </Edit>
    </div>
  );
}
