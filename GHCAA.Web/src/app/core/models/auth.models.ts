export interface LoginDto {
  username: string;
  password: string;
}

export interface TokenResponseDto {
  token: string;
  username: string;
  memberId?: number;
}

export interface User {
  username: string;
  memberId?: number;
  token: string;
}
