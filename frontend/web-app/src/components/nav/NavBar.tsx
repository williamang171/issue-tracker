import LoginButton from './LoginButton';
import { getCurrentUser } from '@app/actions/authActions';

export default async function Navbar() {
  const user = await getCurrentUser();
  return (
    <header className="sticky top-0 z-50 flex justify-between bg-white shadow-md py-5 px-5 items-center text-gray-800">
      {user ? <div></div> : <LoginButton />}
    </header>
  );
}
