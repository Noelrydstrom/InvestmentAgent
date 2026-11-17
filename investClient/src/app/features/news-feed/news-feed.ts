import { Component, signal } from '@angular/core';

type NewsCategory = 'ALL' | 'MARKET' | 'STOCKS' | 'ECONOMY' | 'TECH';

interface NewsItem {
  id: number;
  title: string;
  summary: string;
  source: string;
  time: string;
  category: Exclude<NewsCategory, 'ALL'>;
  impact: 'HIGH' | 'MEDIUM' | 'LOW';
  sentiment: 'POSITIVE' | 'NEGATIVE' | 'NEUTRAL';
  tickers: string[];
}

@Component({
  selector: 'app-news-feed',
  standalone: true,
  templateUrl: './news-feed.html',
  styleUrl: './news-feed.scss'
})
export class NewsFeedComponent {

  selectedCategory = signal<NewsCategory>('ALL');

  categories: NewsCategory[] = [
    'ALL',
    'MARKET',
    'STOCKS',
    'ECONOMY',
    'TECH'
  ];

  news = signal<NewsItem[]>([
    {
      id: 1,
      title: 'NVIDIA reports strong quarterly growth',
      summary: 'Strong demand for AI infrastructure continues to drive revenue growth.',
      source: 'Market Watch',
      time: '2 min ago',
      category: 'TECH',
      impact: 'HIGH',
      sentiment: 'POSITIVE',
      tickers: ['NVDA']
    },
    {
      id: 2,
      title: 'European markets open higher',
      summary: 'Major European indices gain as investor confidence improves.',
      source: 'Reuters',
      time: '12 min ago',
      category: 'MARKET',
      impact: 'MEDIUM',
      sentiment: 'POSITIVE',
      tickers: ['OMXS30', 'DAX']
    },
    {
      id: 3,
      title: 'Inflation concerns pressure bond markets',
      summary: 'Investors reassess expectations around future interest rate decisions.',
      source: 'Bloomberg',
      time: '28 min ago',
      category: 'ECONOMY',
      impact: 'HIGH',
      sentiment: 'NEGATIVE',
      tickers: ['BONDS']
    },
    {
      id: 4,
      title: 'Tesla shares fluctuate after new announcement',
      summary: 'Investors react to updated delivery expectations.',
      source: 'CNBC',
      time: '45 min ago',
      category: 'STOCKS',
      impact: 'MEDIUM',
      sentiment: 'NEUTRAL',
      tickers: ['TSLA']
    },
    {
      id: 5,
      title: 'Microsoft expands AI infrastructure',
      summary: 'New investments signal continued expansion in cloud computing.',
      source: 'Financial Times',
      time: '1 hour ago',
      category: 'TECH',
      impact: 'MEDIUM',
      sentiment: 'POSITIVE',
      tickers: ['MSFT']
    }
  ]);

  get filteredNews(): NewsItem[] {
    const category = this.selectedCategory();

    if (category === 'ALL') {
      return this.news();
    }

    return this.news().filter(item => item.category === category);
  }

  selectCategory(category: NewsCategory): void {
    this.selectedCategory.set(category);
  }
}