import { Component } from '@angular/core';

@Component({
  selector: 'app-messages',
  standalone: true,
  template: `
    <div class="page-header">
      <h1 class="page-header__title">Beskeder</h1>
    </div>
    <div class="page">
      <p>Beskeder vises her.</p>
    </div>
  `
})
export class MessagesComponent {}
