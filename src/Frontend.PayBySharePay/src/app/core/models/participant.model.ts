export interface Participant {
  id: number;
  name: string;
  email: string;
  phone?: string;
  participantType: ParticipantType;
  merchantInfo?: MerchantInfo;
}

export enum ParticipantType {
  Person = 0,
  Merchant = 1
}

export interface MerchantInfo {
  companyName: string;
  cvr?: string;
  vatNumber?: string;
  contactPerson?: string;
  contactEmail?: string;
  contactPhone?: string;
  businessAddress?: string;
  paymentReference?: string;
  payoutInfo?: string;
  paymentProvider?: string;
}

// API response DTO – matcher Service.PayBySharePay.DTOs.ParticipantDto
export interface ParticipantApiDto {
  id: number;
  type: string; // "Person" | "Merchant"
  name: string;
  email?: string;
  phone?: string;
  companyName?: string;
}

export function mapParticipant(dto: ParticipantApiDto): Participant {
  return {
    id: dto.id,
    name: dto.name,
    email: dto.email ?? '',
    phone: dto.phone,
    participantType: dto.type === 'Merchant' ? ParticipantType.Merchant : ParticipantType.Person,
    merchantInfo: dto.companyName ? { companyName: dto.companyName } : undefined
  };
}

export interface CreatePersonRequest {
  name: string;
  email: string;
  phone?: string;
}

export interface CreateMerchantRequest {
  name: string;
  companyName: string;
  cvrNumber?: string;
  vatNumber?: string;
  contactPerson?: string;
  contactEmail?: string;
  contactPhone?: string;
  companyAddress?: string;
  paymentReference?: string;
  payoutAccountInfo?: string;
  paymentProvider?: string;
}

