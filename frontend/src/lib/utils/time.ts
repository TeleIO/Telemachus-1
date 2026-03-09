const SECONDS_PER_DAY = 86400;
const SECONDS_PER_HOUR = 3600;
const SECONDS_PER_MINUTE = 60;
const DAYS_PER_YEAR = 365;

function pad2(n: number): string {
	return n < 10 ? '0' + n : '' + n;
}

export function hourMinSec(v: number): string {
	const hour = Math.floor(v / SECONDS_PER_HOUR);
	v %= SECONDS_PER_HOUR;
	const min = pad2(Math.floor(v / SECONDS_PER_MINUTE));
	const sec = pad2(Math.round(v % SECONDS_PER_MINUTE));
	return `${hour}:${min}:${sec}`;
}

export function durationString(v: number): string {
	v = Math.floor(v);
	const days = Math.floor(v / SECONDS_PER_DAY);
	v %= SECONDS_PER_DAY;
	const hours = pad2(Math.floor(v / SECONDS_PER_HOUR));
	v %= SECONDS_PER_HOUR;
	const mins = pad2(Math.floor(v / SECONDS_PER_MINUTE));
	const secs = pad2(v % SECONDS_PER_MINUTE);
	return `${days}d ${hours}h ${mins}m ${secs}s`;
}

export function dateString(v: number): string {
	const year = Math.floor(v / (DAYS_PER_YEAR * SECONDS_PER_DAY)) + 1;
	v %= DAYS_PER_YEAR * SECONDS_PER_DAY;
	const day = Math.floor(v / SECONDS_PER_DAY) + 1;
	v %= SECONDS_PER_DAY;
	return `Year ${year}, Day ${day}, ${hourMinSec(v)} UT`;
}

export function missionTimeString(v: number): string {
	v = Math.floor(v);
	const hours = pad2(Math.floor(v / SECONDS_PER_HOUR));
	v %= SECONDS_PER_HOUR;
	const mins = pad2(Math.floor(v / SECONDS_PER_MINUTE));
	const secs = pad2(v % SECONDS_PER_MINUTE);
	return `T+${hours}:${mins}:${secs}`;
}
