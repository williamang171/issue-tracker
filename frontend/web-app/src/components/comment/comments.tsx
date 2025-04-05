import type { FC } from 'react';
import { useParams } from 'next/navigation';

import { DeleteButton, useForm } from '@refinedev/antd';
import {
  CanAccess,
  type HttpError,
  useCan,
  useGetIdentity,
  useInvalidate,
  useList,
} from '@refinedev/core';
import { LoadingOutlined } from '@ant-design/icons';
import { Button, Card, Form, Input, Space, Typography } from 'antd';
import { Text } from '@components/text';
import { CustomAvatar } from '@components/custom-avatar';

import { formatTimestamp } from '@app/utils/utils-dayjs';
import { RESOURCE } from '@app/constants/resource';
import { useSession } from 'next-auth/react';
import { useGetUserRole } from '@hooks/useGetUserRole';

type Props = {
  style?: React.CSSProperties;
};

const resource = RESOURCE.comments;

export const Comments: FC<Props> = ({ style }) => {
  return (
    <Card
      bodyStyle={{
        padding: '0',
      }}
      title={
        <Space size={16}>
          <Text>Comments</Text>
        </Space>
      }
      style={style}
    >
      <CommentForm />
      <CommentList />
    </Card>
  );
};

export const CommentForm = () => {
  const params = useParams();
  const issueId = params.id;
  const { data } = useSession();

  const { formProps, onFinish, form, formLoading } = useForm<any, HttpError>({
    action: 'create',
    resource: resource,
    queryOptions: {
      enabled: false,
    },
    redirect: false,
    successNotification: () => ({
      key: 'comment',
      message: 'Successfully added comment',
      description: 'Successful',
      type: 'success',
    }),
  });

  const handleOnFinish = async (values: any) => {
    if (!issueId) {
      return;
    }

    const content = values.content.trim();
    if (!content) {
      return;
    }

    try {
      await onFinish({
        ...values,
        issueId: issueId as string,
      });

      form.resetFields();
    } catch (error) {
      console.log(error);
    }
  };

  const { isAdmin, isMember } = useGetUserRole();
  if (!isAdmin && !isMember) {
    return null;
  }

  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        gap: '12px',
        padding: '1rem',
      }}
    >
      <CustomAvatar
        style={{ flexShrink: 0 }}
        name={data?.user?.username}
      />
      <Form {...formProps} style={{ width: '100%' }} onFinish={handleOnFinish}>
        <Form.Item
          name="content"
          noStyle
          rules={[
            {
              required: true,
              transform(value) {
                return value?.trim();
              },
              message: 'Please enter comment',
            },
          ]}
        >
          <Input
            placeholder="Add your comment"
            addonAfter={formLoading && <LoadingOutlined />}
          />
        </Form.Item>
      </Form>
    </div>
  );
};

export const CommentList = () => {
  const params = useParams();

  const { data } = useList<any>({
    resource: resource,
    pagination: {
      mode: 'off',
    },
    sorters: [
      {
        field: 'createdTime',
        order: 'desc',
      },
    ],
    filters: [{ field: 'issueId', operator: 'eq', value: params.id }],
  });

  const { formProps, setId, id, saveButtonProps } = useForm<
    any,
    HttpError,
    any
  >({
    resource: resource,
    action: 'edit',
    queryOptions: {
      enabled: false,
    },
    redirect: false,
    mutationMode: 'optimistic',
    onMutationSuccess: () => {
      setId(undefined);
    },
    successNotification: () => ({
      key: 'delete-comment',
      message: 'Successfully updated comment',
      description: 'Successful',
      type: 'success',
    }),
  });

  const { data: userData } = useSession();
  const { isAdmin, isMember } = useGetUserRole();
  const username = userData?.user.username;

  return (
    <Space
      size={16}
      direction="vertical"
      style={{
        borderRadius: '8px',
        padding: '1rem',
        width: '100%',
      }}
    >
      {data?.data?.map((item) => {
        const isMe = item.createdBy === username;
        return (
          <div key={item.id} style={{ display: 'flex', gap: '12px' }}>
            <CustomAvatar style={{ flexShrink: 0 }} name={item.createdBy} />

            <div
              style={{
                display: 'flex',
                flexDirection: 'column',
                gap: '8px',
                width: '100%',
              }}
            >
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}
              >
                <Text style={{ fontWeight: 500 }}>{item.createdBy}</Text>
                <Text size="xs">{formatTimestamp(item.createdTime)}</Text>
              </div>

              {id === item.id ? (
                <Form {...formProps} initialValues={{ content: item.content }}>
                  <Form.Item
                    name="content"
                    rules={[
                      {
                        required: true,
                        transform(value) {
                          return value?.trim();
                        },
                        message: 'Please enter comment',
                      },
                    ]}
                  >
                    <Input.TextArea
                      autoFocus
                      required
                      minLength={1}
                      style={{
                        boxShadow:
                          '0px 1px 2px 0px rgba(0, 0, 0, 0.03), 0px 1px 6px -1px rgba(0, 0, 0, 0.02), 0px 2px 4px 0px rgba(0, 0, 0, 0.02)',

                        width: '100%',
                      }}
                    />
                  </Form.Item>
                </Form>
              ) : (
                <Typography.Paragraph
                  style={{
                    marginBottom: 0,
                  }}
                  ellipsis={{ rows: 3, expandable: true }}
                >
                  {item.content}
                </Typography.Paragraph>
              )}
              <Space size={0} >
                {isMe && !id && (
                  <Typography.Link
                    type="secondary"
                    style={{
                      fontSize: '12px',
                      marginRight: '12px',
                    }}
                    onClick={() => setId(item.id)}
                  >
                    Edit
                  </Typography.Link>
                )}
                {((isMe || isAdmin) && !id) && (
                  <DeleteButton
                    accessControl={{ enabled: true }}
                    disabled={false}
                    resource="comments"
                    recordItemId={item.id}
                    size="small"
                    type="link"
                    successNotification={() => ({
                      key: 'delete-comment',
                      message: 'Successfully deleted comment',
                      description: 'Successful',
                      type: 'success',
                    })}
                    icon={null}
                    className="ant-typography secondary"
                    style={{
                      fontSize: '12px',
                      paddingLeft: 0
                    }}
                  />
                )}
              </Space>
              {id === item.id && (
                <Space>
                  <Button size="small" onClick={() => setId(undefined)}>
                    Cancel
                  </Button>
                  <Button size="small" type="primary" {...saveButtonProps}>
                    Save
                  </Button>
                </Space>
              )}
            </div>
          </div>
        );
      })}
    </Space>
  );
};
