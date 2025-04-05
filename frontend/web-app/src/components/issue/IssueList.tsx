'use client';

import {
  ISSUE_PRIORITY,
  ISSUE_PRIORITY_ARRAY,
} from '@app/constants/issue-priority';
import { ISSUE_STATUS, ISSUE_STATUS_ARRAY } from '@app/constants/issue-status';
import { ISSUE_TYPE, ISSUE_TYPE_ARRAY } from '@app/constants/issue-type';
import { PlusSquareOutlined, SearchOutlined } from '@ant-design/icons';
import { renderEnumFieldLabel } from '@app/utils/utils-table-col-render';
import {
  CreateButton,
  DeleteButton,
  EditButton,
  FilterDropdown,
  getDefaultSortOrder,
  List,
  ShowButton,
  useSelect,
  useTable,
} from '@refinedev/antd';
import {
  BaseKey,
  CrudFilter,
  CrudFilters,
  getDefaultFilter,
  useNavigation,
  type BaseRecord,
} from '@refinedev/core';
import { Button, Input, InputRef, Space, Table, theme } from 'antd';
import { RESOURCE } from '@app/constants/resource';
import { useRef, useMemo } from 'react';
import { formatTimestamp } from '@app/utils/utils-dayjs';
import Link from 'next/link';

export default function IssueList({
  assignee,
  projectId,
}: {
  assignee?: string | null;
  projectId?: BaseKey;
}) {
  const permanentFilters: CrudFilters = useMemo(() => {
    const filters: CrudFilters = [];
    if (assignee) {
      filters.push({
        field: 'assignee',
        operator: 'eq',
        value: assignee,
      });
    }
    if (projectId) {
      filters.push({
        field: 'projectId',
        operator: 'eq',
        value: projectId,
      });
    }
    return filters;
  }, [assignee, projectId]);

  const { tableProps, filters, sorters } = useTable({
    syncWithLocation: projectId ? false : true,
    resource: 'issues',
    filters: {
      initial: [
        {
          field: 'name',
          operator: 'contains',
          value: '',
        },
      ],
      permanent: permanentFilters,
    },
    sorters: {
      initial: [
        {
          field: 'createdTime',
          order: 'desc',
        },
      ],
    },
  });

  const { create } = useNavigation();
  const { token } = theme.useToken();
  const { query: queryResult } = useSelect({
    resource: RESOURCE.projects,
    optionLabel: 'name',
    optionValue: 'id',
  });
  const { query: usersQueryResult } = useSelect({
    resource: projectId
      ? `${RESOURCE.projectAssignments}/all?projectId=${projectId}`
      : RESOURCE.users,
    optionLabel: 'userName',
    optionValue: 'userName',
  });
  // const { query: assignedUsersQueryResult } = useSelect({
  //   resource: `${RESOURCE.projectAssignments}/all?projectId=${projectId}`,
  //   optionLabel: 'userName',
  //   optionValue: 'userName',
  // });
  const searchInput = useRef<InputRef>(null);
  const projects = queryResult?.data?.data || [];
  const users = usersQueryResult?.data?.data || [];
  // const assignedUsers = assignedUsersQueryResult?.data?.data || [];
  // const finalUsers = projectId ? assignedUsers : users;
  // <Link href={`/issues/edit/${record.id}?from=${projectId}`}>Details</Link>;
  return (
    <List
      resource="issues"
      breadcrumb={false}
      title="Issues"
      headerButtons={
        <Link
          href={
            projectId ? `/issues/create/?from=${projectId}` : `/issues/create`
          }
        >
          <Button type="primary" icon={<PlusSquareOutlined />}>
            Create
          </Button>
        </Link>
      }
    >
      <Table {...tableProps} rowKey="id">
        <Table.Column
          dataIndex="name"
          title={'Name'}
          filterIcon={(filtered) => (
            <SearchOutlined
              style={{
                color: filtered ? token.colorPrimary : undefined,
              }}
            />
          )}
          sorter
          defaultFilteredValue={getDefaultFilter('name', filters, 'contains')}
          filterDropdown={(props) => (
            <FilterDropdown {...props}>
              <Input placeholder={''} ref={searchInput} />
            </FilterDropdown>
          )}
          filterDropdownProps={{
            onOpenChange: (open) => {
              if (open) {
                setTimeout(() => searchInput.current?.select(), 100);
              }
            },
          }}
        />
        {projectId ? null : (
          <Table.Column
            dataIndex="projectId"
            title="Project"
            render={(_, record) => {
              const found = projects.find(
                (item) => item?.id === record.projectId
              );
              return found?.name;
            }}
            defaultFilteredValue={getDefaultFilter('projectId', filters, 'eq')}
            filterMultiple={false}
            filters={projects.map((item) => {
              return {
                text: item.name,
                value: item.id,
              };
            })}
          />
        )}

        <Table.Column
          dataIndex="status"
          title={'Status'}
          render={(value, record) => {
            return renderEnumFieldLabel(value, record, 'status', ISSUE_STATUS);
          }}
          defaultFilteredValue={getDefaultFilter('status', filters, 'eq')}
          filters={ISSUE_STATUS_ARRAY.map((item) => {
            return {
              text: item.label,
              value: item.id,
            };
          })}
          filterMultiple={false}
        />
        <Table.Column
          dataIndex="priority"
          title={'Priority'}
          render={(value, record) => {
            return renderEnumFieldLabel(
              value,
              record,
              'priority',
              ISSUE_PRIORITY
            );
          }}
          defaultFilteredValue={getDefaultFilter('priority', filters, 'eq')}
          filters={ISSUE_PRIORITY_ARRAY.map((item) => {
            return {
              text: item.label,
              value: item.id,
            };
          })}
          filterMultiple={false}
        />
        <Table.Column
          dataIndex="type"
          title={'Type'}
          render={(value, record) => {
            return renderEnumFieldLabel(value, record, 'type', ISSUE_TYPE);
          }}
          defaultFilteredValue={getDefaultFilter('type', filters, 'eq')}
          filterMultiple={false}
          filters={ISSUE_TYPE_ARRAY.map((item) => {
            return {
              text: item.label,
              value: item.id,
            };
          })}
          filterDropdownProps={{
            onOpenChange: (open) => {
              if (open) {
                setTimeout(() => searchInput.current?.select(), 100);
              }
            },
          }}
        />
        {assignee ? null : (
          <Table.Column
            dataIndex="assignee"
            title={'Assignee'}
            render={(value, record) => {
              return record.assignee || '-';
            }}
            defaultFilteredValue={getDefaultFilter('assignee', filters, 'eq')}
            filterMultiple={false}
            filters={users.map((item) => {
              return {
                text: item.userName,
                value: item.userName,
              };
            })}
          />
        )}
        <Table.Column
          dataIndex="createdTime"
          title="Created At"
          render={(value) => {
            if (!value) return '-';
            return <div>{formatTimestamp(value)}</div>;
          }}
          sorter
          defaultSortOrder={getDefaultSortOrder('createdTime', sorters)}
        />
        <Table.Column
          title={'Actions'}
          dataIndex="actions"
          render={(_, record: BaseRecord) => (
            <Space>
              <Link href={`/issues/edit/${record.id}?from=${projectId}`}>
                Details
              </Link>
              <DeleteButton
                size="small"
                resource={RESOURCE.issues}
                recordItemId={record.id}
                icon={null}
                type="link"
              />
            </Space>
          )}
        />
      </Table>
    </List>
  );
}
