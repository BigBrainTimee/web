export function createUser(data) {
  const name = data.name ?? '';
  const lastName = data.lastName ?? '';

  return {
    id: data.id,
    name,
    lastName,
    fullName: [name, lastName].filter(Boolean).join(' '),
    email: data.email,
    role: data.role,
    createdAt: data.createdAt,
  };
}
