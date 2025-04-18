'use client';

import { AuditFields } from '@components/fields/audit-fields';
import { GoBack } from '@components/goback';
import { useGetUserRole } from '@hooks/useGetUserRole';
import { Edit, useForm, useSelect } from '@refinedev/antd';
import { Alert, Form, Input, Select, Switch } from 'antd';
import { useMemo } from 'react';
import orderBy from 'lodash/orderBy';

const roleOrder = [
  {
    label: "Viewer",
    order: 1
  },
  {
    label: "Member",
    order: 2
  },
  {
    label: "Admin",
    order: 3
  }
];

export default function UserEdit() {
  const { formProps, saveButtonProps, formLoading, query: queryResult, } = useForm({});

  const { selectProps: roleSelectProps } = useSelect({
    resource: 'roles',
    optionLabel: 'name',
    optionValue: 'id'
  });

  const formattedOptions = useMemo(() => {
    if (Array.isArray(roleSelectProps.options)) {
      const mapped = roleSelectProps.options.map(o => {
        return {
          ...o,
          order: roleOrder.find(ro => ro.label === o.label)?.order
        }
      });
      return orderBy(mapped, ["order"]);
    }
    return roleSelectProps.options;
  }, [roleSelectProps.options]);

  const { isAdmin } = useGetUserRole();
  if (!isAdmin) {
    return null;
  }

  if (queryResult?.status === 'error') {
    return <Alert type="error" message="Not Found" />;
  }

  return (
    <div>
      <Edit
        isLoading={formLoading || queryResult?.status === 'loading'}
        title={
          <GoBack
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
            <Select {...roleSelectProps} options={formattedOptions} />
          </Form.Item>
          <Form.Item label={'Is Active'} name={'isActive'}>
            <Switch />
          </Form.Item>
          <AuditFields />
        </Form>
      </Edit>
    </div>
  );
}
