import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';

dayjs.extend(utc);

export function formatTimestamp(isoString: string): string {
  if (!isoString) {
    return '-';
  }
  return dayjs.utc(isoString).local().format('DD MMM YYYY hh:mm A');
}
