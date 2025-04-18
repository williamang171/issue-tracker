import React, { useMemo } from 'react';
import {
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

import { Card } from 'antd';
import { EmptyPlaceholder } from './EmptyPlaceholder';
import { useChartShowEmpty } from '@hooks/useChartShowEmpty';

export const ProjectBarChart = (props: { data: any[]; title: string, isFetched?: boolean }) => {
  const { data, title, isFetched } = props;
  const showEmpty = useChartShowEmpty(data, isFetched);

  return (
    <Card
      style={{ height: '400px', width: '100%', padding: 0 }}
      bodyStyle={{
        padding: '8px 8px 8px 12px',
        height: '100%',
      }}
      size="small"
    >
      <div className="bg-white p-4 rounded-lg shadow-md">
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            whiteSpace: 'nowrap',
          }}
        >
          <div>{title}</div>
        </div>
        {showEmpty ? <EmptyPlaceholder /> : <ResponsiveContainer width="100%" height={300}>
          <BarChart data={data} margin={{ top: 80, right: 40 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis interval={1} />
            <Tooltip formatter={(value) => [`${value} issues`, 'Count']} />

            <Bar dataKey="value" name="Issues" radius={[4, 4, 0, 0]}>
              {data.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.color} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>}

      </div>
    </Card>
  );
};

export default ProjectBarChart;
