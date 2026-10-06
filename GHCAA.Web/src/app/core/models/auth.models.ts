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
  roles?: string[];
}

/** An accepted, live election appointment from /auth/me. 37.12d fills the list. */
export interface ElectionAppointmentSummary {
  electionId: number;
  electionTitle: string;
  personaName: string;
  permissions: number;
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
  /** Every role the user holds. Missing on a session saved before 37.12c. */
  roles?: string[];
  electionAppointments?: ElectionAppointmentSummary[];
}

// One row of GET api/auth/providers. The server lists only providers that can sign in now (7.17).
export interface SocialProviderConfig {
    provider: 'Google' | 'Facebook';
    clientId: string;
}
