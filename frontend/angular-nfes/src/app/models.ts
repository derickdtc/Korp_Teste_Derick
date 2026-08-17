export interface Produto { id: string; codigo: string; descricao: string; saldo: number; }

export interface ItemNotaFiscal { produtoId: string; quantidade: number; }

export interface NotaFiscal { id: string; numero: number; status: 'Aberta' | 'Fechada'; itens: ItemNotaFiscal[]; }

export interface ApiError { statusCode?: number; message?: string; }
