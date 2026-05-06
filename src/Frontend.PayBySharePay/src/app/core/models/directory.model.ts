export type DirectoryEntryType = 'Person' | 'Merchant';

export interface DirectoryEntry {
  id: number;
  type: DirectoryEntryType;
  displayName: string;
  handle?: string;
  subtitle?: string;
  logoUrl?: string;
}
