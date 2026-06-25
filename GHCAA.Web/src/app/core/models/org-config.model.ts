export interface OrgConfig {
  orgId: string;
  themeId?: number;
  features: {
    enableEBook: boolean;
    enableAlumniGallery: boolean;
    enableDiscussionForums: boolean;
    enableJobBoard: boolean;
    enableAcademicRecords: boolean;
    enableProfessionalRecords: boolean;
  };
  contact: {
    email: string;
    phone: string;
    address: string;
    socialLinks: {
      facebook?: string;
      whatsapp?: string;
      youtube?: string;
    };
  };
}
