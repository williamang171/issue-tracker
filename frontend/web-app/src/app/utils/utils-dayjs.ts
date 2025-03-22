import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';

dayjs.extend(utc);

export function formatTimestamp(isoString: string): string {
  return dayjs.utc(isoString).local().format('DD MMM YYYY hh:mm A');
}

// const timestamp = "2024-08-27T12:55:46.918569Z";
// const readableDate = formatTimestamp(timestamp);
// console.log(readableDate);
