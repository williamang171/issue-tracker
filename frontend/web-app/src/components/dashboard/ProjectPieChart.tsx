import React from 'react';
import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Tooltip,
  Legend,
} from 'recharts';
import { Card } from 'antd';

export const ProjectPieChart = (props: { data: any[]; title: string }) => {
  const { data, title } = props;
  return (
    <Card
      style={{ height: '400px', width: '100%', padding: 0 }}
      bodyStyle={{
        padding: '8px 8px 8px 12px',
        height: '100%',
      }}
      size="small"
    >
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
      <div
        style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: '100%',
          width: '100%',
        }}
      >
        <ResponsiveContainer width={300} height={300}>
          <PieChart width={300} height={300}>
            <Pie
              data={data}
              cx="50%"
              cy="50%"
              // labelLine={false}
              dataKey="value"
            >
              {data.map((entry, index) => {
                return <Cell key={`cell-${index}`} fill={entry.color} />;
              })}
            </Pie>
            <Tooltip />
            <Legend layout="vertical" align="center" verticalAlign="bottom" />
          </PieChart>
        </ResponsiveContainer>
      </div>
    </Card>
  );
};

export default ProjectPieChart;
