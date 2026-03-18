export interface LoginDto {
  username: string;
  password: string;
}

export interface TokenResponseDto {
  token: string;
  username: string;
  memberId?: number;
  role: string;
  fullName?: string;
  email?: string;
  mobileNo?: string;
  mustChangePassword?: boolean;
}

export interface User {
  username: string;
  memberId?: number;
  token: string;
  role: string;
  fullName?: string;
  email?: string;
  mobileNo?: string;
  mustChangePassword?: boolean;
}
