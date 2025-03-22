import { useSelect } from '@refinedev/antd';
import { Form, Select } from 'antd';

export default function IssueFormItem({ projectId }: { projectId: string }) {
  const { selectProps: assigneeSelectProps } = useSelect({
    resource: 'projectAssignments/all',
    optionLabel: 'userName',
    optionValue: 'userName',
    filters: [
      {
        field: 'projectId',
        operator: 'eq',
        value: projectId,
      },
    ],
  });

  return (
    <Form.Item label={'Assignee'} name={['assignee']}>
      <Select {...assigneeSelectProps} style={{ width: 200 }} />
    </Form.Item>
  );
}
