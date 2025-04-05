'use client';

import { ISSUE_PRIORITY_ARRAY } from '@app/constants/issue-priority';
import { ISSUE_STATUS_ARRAY } from '@app/constants/issue-status';
import { ISSUE_TYPE_ARRAY } from '@app/constants/issue-type';
import { mapToSelectItemObject } from '@app/utils/uitils-select';
import { Edit, useForm, useSelect } from '@refinedev/antd';
import { Col, Form, Input, Row, Select } from 'antd';
import AssigneeFormItem from './AssigneeFormItem';
import { Comments } from '@components/comment/comments';
import { AttachmentList } from '@components/attachment/list';
import { BaseRecord, useMeta, useNavigation } from '@refinedev/core';
import { RESOURCE } from '@app/constants/resource';
import { useSearchParams } from 'next/navigation';
import { GoBack } from '@components/goback';
import { useSession } from 'next-auth/react';
import { useGetUserRole } from '@hooks/useGetUserRole';

export default function IssueEdit() {
  const searchParams = useSearchParams();
  const projectId = searchParams.get('projectId');
  const projectName = searchParams.get('projectName');
  const {
    formProps,
    saveButtonProps,
    query: queryResult,
    onFinish,
  } = useForm({
    redirect: projectId ? false : 'list',
  });
  const { edit } = useNavigation();

  const { selectProps: projectSelectProps } = useSelect({
    resource: 'projects/all',
    optionLabel: 'name',
  });
  const id = queryResult?.data?.data.id;
  const { data } = useSession();
  const handleOnFinish = async (values: BaseRecord) => {
    await onFinish({
      ...values,
      unassignUser: values.assignee === undefined,
    });
    if (typeof projectId === 'string') {
      edit(RESOURCE.projects, projectId);
    }
  };
  const { isAdmin, isReadOnly } = useGetUserRole();

  return (
    <div>
      <Edit
        deleteButtonProps={{
          disabled: !isAdmin && queryResult?.data?.data.createdBy !== data?.user.username
        }}
        saveButtonProps={{
          ...saveButtonProps,
          disabled: isReadOnly
        }}
        breadcrumb={false}
        headerButtons={<div />}
        title={
          <GoBack
            title='Issue Details'
            href={projectId ? `/projects/edit/${projectId}` : `/issues`}
          />
        }
        goBack={null}
      >
        <Form {...formProps} layout="vertical" onFinish={handleOnFinish} disabled={isReadOnly}>
          <Form.Item
            label={'Project'}
            name={'projectId'}
            rules={[
              {
                required: true,
              },
            ]}
          >
            <Select {...projectSelectProps} disabled labelRender={(props) => {
              if (props.label) {
                return props.label;
              }
              return '';
            }} />
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
            <Input.TextArea rows={2} />
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
          <AssigneeFormItem projectId={queryResult?.data?.data.projectId} />
        </Form>
      </Edit>
      <div style={{ marginBottom: '24px' }} />
      <Row gutter={[24, 24]}>
        <Col xs={24} sm={24} md={12}>
          <Comments />
        </Col>
        <Col xs={24} sm={24} md={12}>
          <AttachmentList />
        </Col>
      </Row>
    </div>
  );
}
