export function createUser(data) {
  return {
    id: data.id,
    name: data.name,
    email: data.email,
    role: data.role,
    createdAt: data.createdAt,
  };
}
