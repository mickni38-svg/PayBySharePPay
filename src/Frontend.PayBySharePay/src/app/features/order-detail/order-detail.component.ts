import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import { Order, OrderParticipant, OrderParticipantStatus, OrderStatus, OrderOverviewApiDto, mapOrderParticipantStatus } from "../../core/models/order.model";
import { ParticipantType } from "../../core/models/participant.model";
import { OrderService } from "../../core/services/order.service";

interface CategoryOption { icon: string; label: string; key: string; }
interface OrderLine { name: string; quantity: number; unitPrice: number; }
interface ParticipantVM extends OrderParticipant { initials: string; avatarColor: string; orderLines: OrderLine[]; }

const AVATAR_COLORS = ["#7c5cbf","#2e7d32","#1565c0","#ad1457","#00838f","#558b2f","#4527a0","#6d4c41"];

function avatarColor(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

function toInitials(name: string): string {
  return name.split(" ").slice(0, 2).map(p => p[0]).join("").toUpperCase();
}



const CATEGORIES: CategoryOption[] = [
  { key: "sushi", icon: "🍣", label: "Sushi" }, { key: "pizza", icon: "🍕", label: "Pizza" },
  { key: "burger", icon: "🍔", label: "Burger" }, { key: "drinks", icon: "🍺", label: "Drinks" },
  { key: "tacos", icon: "🌮", label: "Tacos" }, { key: "ramen", icon: "🍜", label: "Ramen" },
  { key: "kebab", icon: "🥙", label: "Kebab" }, { key: "chicken", icon: "🍗", label: "Kylling" },
  { key: "salad", icon: "🥗", label: "Salat" }, { key: "dessert", icon: "🍰", label: "Dessert" },
  { key: "coffee", icon: "☕", label: "Kaffe" }, { key: "other", icon: "📦", label: "Andet" },
];

@Component({
  selector: "app-order-detail",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./order-detail.component.html",
  styleUrl: "./order-detail.component.scss"
})
export class OrderDetailComponent implements OnInit {
  readonly ParticipantType = ParticipantType;
  readonly OrderParticipantStatus = OrderParticipantStatus;

  categories = CATEGORIES;
  order = signal<Order | null>(null);
  participants = signal<ParticipantVM[]>([]);
  expandedIds = signal<Set<number>>(new Set());
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  constructor(private route: ActivatedRoute, private router: Router, private orderService: OrderService) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get("id"));
    if (!id) { this.router.navigate(["/orders"]); return; }
    this.orderService.getOrderOverview(id).subscribe({
      next: (dto: OrderOverviewApiDto) => {
        this.order.set({
          id: dto.orderId, title: dto.title, category: dto.category, message: dto.message,
          createdDate: new Date(dto.createdAt), createdByParticipantId: 0,
          status: dto.status as OrderStatus,
          orderParticipants: dto.participants.map((p, i) => ({
            id: i + 1, orderId: dto.orderId, participantId: p.participantId, participantName: p.name,
            participantType: p.type === "Merchant" ? ParticipantType.Merchant : ParticipantType.Person,
            status: mapOrderParticipantStatus(p.status)
          }))
        });
        const vms = dto.participants.filter(p => p.type !== "Merchant").map((p, i) => ({
            id: i + 1, orderId: dto.orderId, participantId: p.participantId, participantName: p.name,
            participantType: ParticipantType.Person, status: mapOrderParticipantStatus(p.status),
            initials: toInitials(p.name), avatarColor: avatarColor(p.name),
            orderLines: []
          }));
        this.participants.set(vms);
        this.expandedIds.set(new Set());
        this.isLoading.set(false);
      },
      error: () => { this.errorMessage.set("Kunne ikke hente ordre."); this.isLoading.set(false); }
    });
  }

  activeCategoryIcon(): string { return CATEGORIES.find(c => c.key === this.order()?.category)?.icon ?? ""; }
  activeCategoryLabel(): string { return CATEGORIES.find(c => c.key === this.order()?.category)?.label ?? ""; }

  toggleExpand(id: number): void {
    this.expandedIds.update(set => {
      const next = new Set(set);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  isExpanded(id: number): boolean {
    return this.expandedIds().has(id);
  }

  lineTotal(lines: OrderLine[]): number { return lines.reduce((sum, l) => sum + l.quantity * l.unitPrice, 0); }

  payOrder(): void { alert("Betaling registreres - implementeres i naeste step"); }

  goBack(): void { this.router.navigate(["/orders"]); }
}
