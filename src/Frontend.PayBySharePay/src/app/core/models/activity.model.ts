export type ActivityType = 'payment' | 'participant' | 'order' | 'message';

export interface ActivityItem {
  id: string;
  type: ActivityType;
  icon: string;
  title: string;
  orderName?: string;
  orderId?: number;
  createdAt: string; // ISO string
}

export interface ActivityFeed {
  items: ActivityItem[];
  unreadCount: number;
}
